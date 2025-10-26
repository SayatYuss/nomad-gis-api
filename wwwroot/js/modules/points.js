import { apiFetch } from './api.js';
import { paginationState, renderTable, setupSearch, openModal, closeModal } from './ui.js';

const entityType = 'points';
const pointForm = document.getElementById("point-form");

export function initPoints() {
    setupSearch(entityType, 'point-search', attachEventHandlers);
    document.getElementById("show-create-point-modal").addEventListener("click", () => showPointModal("create"));
    pointForm.addEventListener("submit", handlePointFormSubmit);
    // console.log("Points module initialized");
}

export async function loadMapPoints() {
    const points = await apiFetch("/api/v1/points");
    paginationState[entityType].data = points || [];
    renderTable(entityType, attachEventHandlers);
}

export function attachEventHandlers(container) {
    container.querySelectorAll(".btn-edit-point").forEach(button => {
        button.addEventListener("click", (e) => {
            const pointId = e.target.dataset.pointId;
            const point = paginationState[entityType].data.find(p => p.id === pointId);
            if(point) showPointModal("edit", point);
        });
    });
    container.querySelectorAll(".btn-delete-point").forEach(button => {
        button.addEventListener("click", (e) => deleteMapPoint(e.target.dataset.pointId));
    });
}

function showPointModal(mode, point = null) {
    const modalTitle = document.getElementById("point-modal-title");
    pointForm.reset();
    document.getElementById("point-id").value = "";

    if (mode === "create") {
        modalTitle.textContent = "Создать точку";
    } else {
        modalTitle.textContent = "Редактировать точку";
        document.getElementById("point-id").value = point.id;
        document.getElementById("point-name").value = point.name;
        document.getElementById("point-lat").value = point.latitude;
        document.getElementById("point-lon").value = point.longitude;
        document.getElementById("point-radius").value = point.unlockRadiusMeters;
        document.getElementById("point-desc").value = point.description || "";
    }
    openModal("point-modal");
}

async function handlePointFormSubmit(e) {
    e.preventDefault();
    const id = document.getElementById("point-id").value;
    const isEdit = !!id;
    const body = {
        name: document.getElementById("point-name").value,
        latitude: parseFloat(document.getElementById("point-lat").value),
        longitude: parseFloat(document.getElementById("point-lon").value),
        unlockRadiusMeters: parseFloat(document.getElementById("point-radius").value),
        description: document.getElementById("point-desc").value,
    };
    const endpoint = isEdit ? `/api/v1/points/${id}` : "/api/v1/points";
    const method = isEdit ? "PUT" : "POST";

    const result = await apiFetch(endpoint, { method, body });
    if (result) {
        closeModal("point-modal");
        loadMapPoints(); // Перезагружаем и рендерим
    }
}

async function deleteMapPoint(pointId) {
    if (!confirm(`Удалить точку ${pointId}?`)) return;
    const result = await apiFetch(`/api/v1/points/${pointId}`, { method: "DELETE" });
    if (result) {
        paginationState[entityType].data = paginationState[entityType].data.filter(p => p.id !== pointId);
        renderTable(entityType, attachEventHandlers); // Перерисовываем
    }
}