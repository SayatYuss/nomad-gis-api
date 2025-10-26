document.addEventListener("DOMContentLoaded", () => {
    const token = localStorage.getItem("adminToken");
    if (!token) {
        window.location.href = "/login.html";
        return;
    }

    const headers = {
        "Authorization": `Bearer ${token}`
    };
    
    // V-- API HELPER (ОБНОВЛЕН ДЛЯ FormData) --V
    async function apiFetch(endpoint, options = {}) {
        const defaultHeaders = { ...headers }; 
        let body = options.body;
        
        if (body && !(body instanceof FormData)) {
            body = JSON.stringify(body);
            defaultHeaders['Content-Type'] = 'application/json';
        } else {
            delete defaultHeaders['Content-Type'];
        }
        
        options.headers = { ...defaultHeaders, ...options.headers };
        options.body = body; 

        try {
            const response = await fetch(endpoint, options);
            if (response.status === 401 || response.status === 403) {
                logout();
                return;
            }
            if (!response.ok) {
                 const errorData = await response.json();
                 alert(`Ошибка API: ${errorData.message || response.statusText}`);
                 return null;
            }
            if (response.status === 204) { return true; }
            return await response.json();
        } catch (error) {
            console.error("API Fetch Error:", error);
            alert("Сетевая ошибка или сервер недоступен.");
            return null;
        }
    }
    // ^-- КОНЕЦ ОБНОВЛЕНИЯ API HELPER --^

    // --- Управление Табами (Вкладками) ---
    const tabs = document.querySelectorAll(".tab-link");
    const tabContents = document.querySelectorAll(".tab-content");

    tabs.forEach(tab => {
        tab.addEventListener("click", () => {
            const targetTab = tab.dataset.tab;
            tabs.forEach(t => t.classList.remove("active"));
            tab.classList.add("active");
            tabContents.forEach(c => c.classList.remove("active"));
            document.getElementById(`${targetTab}-tab`).classList.add("active");

            if (targetTab === "dashboard") loadDashboard();
            if (targetTab === "users") loadUsers();
            if (targetTab === "points") loadMapPoints();
            if (targetTab === "achievements") loadAchievements();
            if (targetTab === "moderation") loadPointsForModeration();
        });
    });

    // --- Выход ---
    document.getElementById("logout-button").addEventListener("click", logout);
    function logout() {
        localStorage.removeItem("adminToken");
        window.location.href = "/login.html";
    }

    // --- Дашборд ---
    async function loadDashboard() {
         // ... (код дашборда без изменений)
        const stats = await apiFetch("/api/v1/dashboard/stats");
        if (!stats) {
            document.getElementById("stat-total-users").textContent = "Ошибка";
            document.getElementById("stat-total-points").textContent = "Ошибка";
            document.getElementById("stat-total-messages").textContent = "Ошибка";
            document.getElementById("stat-total-unlocks").textContent = "Ошибка";
            document.getElementById("stat-total-achievements").textContent = "Ошибка";
            return;
        }
        document.getElementById("stat-total-users").textContent = stats.totalUsers;
        document.getElementById("stat-new-users").textContent = `${stats.newUsersToday} новых сегодня`;
        document.getElementById("stat-total-points").textContent = stats.totalMapPoints;
        document.getElementById("stat-total-messages").textContent = stats.totalMessages;
        document.getElementById("stat-new-messages").textContent = `${stats.newMessagesToday} новых сегодня`;
        document.getElementById("stat-total-unlocks").textContent = stats.totalUnlocks;
        document.getElementById("stat-total-achievements").textContent = stats.totalAchievementsWon;
    }

    // --- Управление Пользователями ---
    const usersTableContainer = document.getElementById("users-table-container");

    // V-- ОБНОВЛЕНА: loadUsers (добавлена кнопка "Инфо") --V
    async function loadUsers() {
        const users = await apiFetch("/api/v1/users");
        if (!users) return;

        let html = `
            <table>
                <thead>
                    <tr>
                        <th>avatarUrl</th>
                        <th>ID</th>
                        <th>Username</th>
                        <th>Email</th>
                        <th>Role</th>
                        <th>Действия</th>
                    </tr>
                </thead>
                <tbody>
                    ${users.map(user => `
                        <tr>
                            <td>${user.avatarUrl}</td>
                            <td>${user.id}</td>
                            <td>${user.username}</td>
                            <td>${user.email}</td>
                            <td>
                                <select class="role-select" data-user-id="${user.id}">
                                    <option value="User" ${user.role === 'User' ? 'selected' : ''}>User</option>
                                    <option value="Admin" ${user.role === 'Admin' ? 'selected' : ''}>Admin</option>
                                </select>
                            </td>
                            <td>
                                <button class="btn-secondary btn-view-user" data-user-id="${user.id}">Инфо</button>
                                <button class="btn-danger btn-delete-user" data-user-id="${user.id}">Удалить</button>
                            </td>
                        </tr>
                    `).join("")}
                </tbody>
            </table>
        `;
        usersTableContainer.innerHTML = html;
        
        // Навешиваем обработчики на новые кнопки
        document.querySelectorAll(".role-select").forEach(select => {
            select.addEventListener("change", (e) => {
                const newRole = e.target.value;
                const userId = e.target.dataset.userId;
                updateUserRole(userId, newRole);
            });
        });
        
        document.querySelectorAll(".btn-delete-user").forEach(button => {
            button.addEventListener("click", (e) => {
                const userId = e.target.dataset.userId;
                deleteUser(userId);
            });
        });
        
        // V-- НОВЫЙ ОБРАБОТЧИК ДЛЯ КНОПКИ "ИНФО" --V
        document.querySelectorAll(".btn-view-user").forEach(button => {
            button.addEventListener("click", (e) => {
                const userId = e.target.dataset.userId;
                showUserDetailModal(userId);
            });
        });
        // ^-- КОНЕЦ НОВОГО ОБРАБОТЧИКА --^
    }
    // ^-- КОНЕЦ ОБНОВЛЕНИЯ --^


    async function updateUserRole(userId, role) {
         // ... (код без изменений)
        if (!confirm(`Изменить роль пользователя ${userId} на ${role}?`)) return;
        const result = await apiFetch(`/api/v1/users/${userId}/role`, {
            method: "PUT",
            body: { role }
        });
        if (result) {
            alert("Роль обновлена!");
            loadUsers();
        }
    }
    async function deleteUser(userId) {
         // ... (код без изменений)
        if (!confirm(`Вы уверены, что хотите УДАЛИТЬ пользователя ${userId}? Это действие необратимо.`)) return;
        const result = await apiFetch(`/api/v1/users/${userId}`, { method: "DELETE" });
        if (result) {
            alert("Пользователь удален.");
            loadUsers();
        }
    }

    // V-- НОВЫЙ БЛОК: МОДАЛЬНОЕ ОКНО ДЕТАЛЕЙ ПОЛЬЗОВАТЕЛЯ --V
    const userDetailModal = document.getElementById("user-detail-modal");
    const userDetailContent = document.getElementById("user-detail-content");

    async function showUserDetailModal(userId) {
        // 1. Показать модалку с загрузчиком
        userDetailContent.innerHTML = "<p>Загрузка...</p>";
        userDetailModal.style.display = "block";

        // 2. Загрузить данные
        const user = await apiFetch(`/api/v1/users/${userId}/details`);
        if (!user) {
            userDetailContent.innerHTML = "<p>Ошибка загрузки данных пользователя.</p>";
            return;
        }

        // 3. Сгенерировать HTML для открытых точек
        let pointsHtml = '<p>Пока нет открытых точек.</p>';
        if (user.unlockedPoints && user.unlockedPoints.length > 0) {
            pointsHtml = `
                <ul class="detail-list">
                    ${user.unlockedPoints.map(p => `
                        <li>
                            <span>${p.mapPointName}</span>
                            <span class="date">${new Date(p.unlockedAt).toLocaleString()}</span>
                        </li>
                    `).join("")}
                </ul>
            `;
        }

        // 4. Сгенерировать HTML для достижений
        let achievementsHtml = '<p>Пока нет достижений.</p>';
        if (user.achievements && user.achievements.length > 0) {
            achievementsHtml = `
                <ul class="detail-list">
                    ${user.achievements.map(a => `
                        <li>
                            <span>${a.achievementTitle}</span>
                            <span class="date">${a.isCompleted ? new Date(a.completedAt).toLocaleString() : 'В процессе'}</span>
                        </li>
                    `).join("")}
                </ul>
            `;
        }

        // 5. Собрать все вместе и вставить в модальное окно
        userDetailContent.innerHTML = `
            <div class="user-detail-header">
                <img src="${user.avatarUrl || 'placeholder.jpg'}" alt="Avatar" class="user-detail-avatar">
                <div class="user-detail-info">
                    <h4>${user.username}</h4>
                    <p>${user.email}</p>
                    <p>Роль: ${user.role} | Активен: ${user.isActive ? 'Да' : 'Нет'}</p>
                    
                    <div class="user-detail-stats">
                        <div class="user-detail-stat">
                            <span>${user.level}</span>
                            <small>Уровень</small>
                        </div>
                        <div class="user-detail-stat">
                            <span>${user.experience}</span>
                            <small>Опыт</small>
                        </div>
                    </div>
                </div>
            </div>

            <div class="detail-grid">
                <div class="detail-list-container">
                    <h5>Открытые точки (${user.unlockedPoints.length})</h5>
                    ${pointsHtml}
                </div>
                <div class="detail-list-container">
                    <h5>Достижения (${user.achievements.length})</h5>
                    ${achievementsHtml}
                </div>
            </div>
        `;
    }
    // ^-- КОНЕЦ НОВОГО БЛОКА --^


    // --- Управление Точками (Map Points) ---
    const pointsTableContainer = document.getElementById("points-table-container");
    const pointModal = document.getElementById("point-modal");
    const pointForm = document.getElementById("point-form");
    async function loadMapPoints() {
         // ... (код без изменений)
        const points = await apiFetch("/api/v1/points");
        if (!points) return;
        pointsTableContainer.innerHTML = `
             <table>
                <thead> <tr> <th>ID</th> <th>Название</th> <th>Coords (Lat, Lon)</th> <th>Радиус</th> <th>Действия</th> </tr> </thead>
                <tbody>
                    ${points.map(point => `
                        <tr>
                            <td>${point.id}</td>
                            <td>${point.name}</td>
                            <td>${point.latitude.toFixed(4)}, ${point.longitude.toFixed(4)}</td>
                            <td>${point.unlockRadiusMeters} м.</td>
                            <td>
                                <button class="btn-edit btn-edit-point" data-point-id="${point.id}">Редакт.</button>
                                <button class="btn-danger btn-delete-point" data-point-id="${point.id}">Удалить</button>
                            </td>
                        </tr>
                    `).join("")}
                </tbody>
            </table>
        `;
        document.querySelectorAll(".btn-edit-point").forEach(button => {
            button.addEventListener("click", (e) => {
                const point = points.find(p => p.id === e.target.dataset.pointId);
                showPointModal("edit", point);
            });
        });
        document.querySelectorAll(".btn-delete-point").forEach(button => {
            button.addEventListener("click", (e) => {
                deleteMapPoint(e.target.dataset.pointId);
            });
        });
    }
    function showPointModal(mode, point = null) {
         // ... (код без изменений)
        const modalTitle = document.getElementById("point-modal-title");
        if (mode === "create") {
            modalTitle.textContent = "Создать точку";
            pointForm.reset();
            document.getElementById("point-id").value = "";
        } else {
            modalTitle.textContent = "Редактировать точку";
            document.getElementById("point-id").value = point.id;
            document.getElementById("point-name").value = point.name;
            document.getElementById("point-lat").value = point.latitude;
            document.getElementById("point-lon").value = point.longitude;
            document.getElementById("point-radius").value = point.unlockRadiusMeters;
            document.getElementById("point-desc").value = point.description || "";
        }
        pointModal.style.display = "block";
    }
    document.getElementById("show-create-point-modal").addEventListener("click", () => showPointModal("create"));
    pointForm.addEventListener("submit", async (e) => {
         // ... (код без изменений)
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
        const result = await apiFetch(endpoint, { method, body: body });
        if (result) {
            pointModal.style.display = "none";
            loadMapPoints();
        }
    });
    async function deleteMapPoint(pointId) {
         // ... (код без изменений)
        if (!confirm(`Удалить точку ${pointId}?`)) return;
        const result = await apiFetch(`/api/v1/points/${pointId}`, { method: "DELETE" });
        if (result) {
            loadMapPoints();
        }
    }
    
    // --- Управление Достижениями (Achievements) ---
    const achievementsTableContainer = document.getElementById("achievements-table-container");
    const achievementModal = document.getElementById("achievement-modal");
    const achievementForm = document.getElementById("achievement-form");
    async function loadAchievements() {
         // ... (код без изменений)
        const achievements = await apiFetch("/api/v1/achievements");
        if (!achievements) return;
        achievementsTableContainer.innerHTML = `
             <table>
                <thead> <tr> <th>Значок</th> <th>ID</th> <th>Код</th> <th>Название</th> <th>Награда (опыт)</th> <th>Действия</th> </tr> </thead>
                <tbody>
                    ${achievements.map(ach => `
                        <tr>
                            <td><img src="${ach.badgeImageUrl || ''}" class="table-badge-icon" alt=""></td>
                            <td>${ach.id}</td>
                            <td>${ach.code}</td>
                            <td>${ach.title}</td>
                            <td>${ach.rewardPoints}</td>
                            <td>
                                <button class="btn-edit btn-edit-achievement" data-achievement-id="${ach.id}">Редакт.</button>
                                <button class="btn-danger btn-delete-achievement" data-achievement-id="${ach.id}">Удалить</button>
                            </td>
                        </tr>
                    `).join("")}
                </tbody>
            </table>
        `;
        document.querySelectorAll(".btn-edit-achievement").forEach(button => {
            button.addEventListener("click", (e) => {
                const achievement = achievements.find(a => a.id === e.target.dataset.achievementId);
                showAchievementModal("edit", achievement);
            });
        });
        document.querySelectorAll(".btn-delete-achievement").forEach(button => {
            button.addEventListener("click", (e) => {
                deleteAchievement(e.target.dataset.achievementId);
            });
        });
    }
    function showAchievementModal(mode, achievement = null) {
         // ... (код без изменений)
        const modalTitle = document.getElementById("achievement-modal-title");
        achievementForm.reset(); 
        const preview = document.getElementById("achievement-badge-preview");
        document.getElementById("achievement-badge").value = null; 
        if (mode === "create") {
            modalTitle.textContent = "Создать достижение";
            document.getElementById("achievement-id").value = "";
            document.getElementById("achievement-code").disabled = false;
            preview.src = "";
            preview.style.display = "none";
        } else {
            modalTitle.textContent = "Редактировать достижение";
            document.getElementById("achievement-id").value = achievement.id;
            document.getElementById("achievement-code").value = achievement.code;
            document.getElementById("achievement-code").disabled = true; 
            document.getElementById("achievement-title").value = achievement.title;
            document.getElementById("achievement-desc").value = achievement.description || "";
            document.getElementById("achievement-reward").value = achievement.rewardPoints;
            if (achievement.badgeImageUrl) {
                preview.src = achievement.badgeImageUrl;
                preview.style.display = "block";
            } else {
                preview.src = "";
                preview.style.display = "none";
            }
        }
        achievementModal.style.display = "block";
    }
    document.getElementById("show-create-achievement-modal").addEventListener("click", () => showAchievementModal("create"));
    achievementForm.addEventListener("submit", async (e) => {
         // ... (код без изменений, использующий FormData)
        e.preventDefault();
        const id = document.getElementById("achievement-id").value;
        const isEdit = !!id;
        const formData = new FormData();
        formData.append("Title", document.getElementById("achievement-title").value);
        formData.append("Description", document.getElementById("achievement-desc").value);
        formData.append("RewardPoints", parseInt(document.getElementById("achievement-reward").value, 10));
        const badgeFile = document.getElementById("achievement-badge").files[0];
        if (badgeFile) {
            formData.append("BadgeFile", badgeFile);
        }
        let endpoint, method;
        if (isEdit) {
            endpoint = `/api/v1/achievements/${id}`;
            method = "PUT";
        } else {
            endpoint = "/api/v1/achievements";
            method = "POST";
            formData.append("Code", document.getElementById("achievement-code").value);
        }
        const result = await apiFetch(endpoint, { method, body: formData }); 
        if (result) {
            achievementModal.style.display = "none";
            loadAchievements();
        }
    });
    async function deleteAchievement(achievementId) {
         // ... (код без изменений)
        if (!confirm(`Удалить достижение ${achievementId}?`)) return;
        const result = await apiFetch(`/api/v1/achievements/${achievementId}`, { method: "DELETE" });
        if (result) {
            loadAchievements();
        }
    }

    // --- Модерация Сообщений ---
    const pointSelect = document.getElementById("point-select");
    const messagesTableContainer = document.getElementById("messages-table-container");
    async function loadPointsForModeration() {
         // ... (код без изменений)
        const points = await apiFetch("/api/v1/points");
        if (!points) {
            pointSelect.innerHTML = "<option value=''>Не удалось загрузить точки</option>";
            return;
        }
        pointSelect.innerHTML = `
            <option value="">-- Выберите точку --</option>
            ${points.map(p => `<option value="${p.id}">${p.name} (ID: ${p.id.substring(0, 8)}...)</option>`).join("")}
        `;
        messagesTableContainer.innerHTML = `<p class="placeholder-text">Выберите точку, чтобы увидеть сообщения.</p>`;
    }
    pointSelect.addEventListener("change", (e) => {
         // ... (код без изменений)
        const pointId = e.target.value;
        if (pointId) {
            loadMessagesForPoint(pointId);
        } else {
             messagesTableContainer.innerHTML = `<p class="placeholder-text">Выберите точку, чтобы увидеть сообщения.</p>`;
        }
    });
    async function loadMessagesForPoint(pointId) {
         // ... (код без изменений)
        const messages = await apiFetch(`/api/v1/messages/point/${pointId}`);
        if (!messages) return;
        if (messages.length === 0) {
            messagesTableContainer.innerHTML = `<p class="placeholder-text">На этой точке нет сообщений.</p>`;
            return;
        }
        messagesTableContainer.innerHTML = `
            <table>
                <thead> <tr> <th>Автор</th> <th>Сообщение</th> <th>Дата</th> <th>Лайки</th> <th>Действия</th> </tr> </thead>
                <tbody>
                    ${messages.map(msg => `
                        <tr>
                            <td>${msg.username}<br><small>(${msg.userId})</small></td>
                            <td class="message-content">${msg.content}</td>
                            <td>${new Date(msg.createdAt).toLocaleString()}</td>
                            <td>${msg.likesCount}</td>
                            <td> <button class="btn-danger btn-delete-message" data-message-id="${msg.id}">Удалить</e> </td>
                        </tr>
                    `).join("")}
                </tbody>
            </table>
        `;
        document.querySelectorAll(".btn-delete-message").forEach(button => {
            button.addEventListener("click", (e) => {
                const messageId = e.target.dataset.messageId;
                deleteMessage(messageId);
            });
        });
    }
    async function deleteMessage(messageId) {
         // ... (код без изменений)
        if (!confirm(`Удалить это сообщение? Действие необратимо.`)) return;
        const result = await apiFetch(`/api/v1/messages/admin/${messageId}`, { method: "DELETE" });
        if (result) {
            const currentPointId = pointSelect.value;
            if (currentPointId) {
                loadMessagesForPoint(currentPointId);
            }
        }
    }


    // --- Утилиты для Модальных окон ---
    document.querySelectorAll(".modal .close-btn").forEach(btn => {
        btn.addEventListener("click", (e) => {
            // V-- ОБНОВЛЕНО: закрывает ЛЮБОЕ модальное окно --V
            document.getElementById(e.target.dataset.modal).style.display = "none";
        });
    });
    window.addEventListener("click", (e) => {
        if (e.target.classList.contains("modal")) {
            e.target.style.display = "none";
        }
    });

    // --- Инициализация ---
    loadDashboard();
});