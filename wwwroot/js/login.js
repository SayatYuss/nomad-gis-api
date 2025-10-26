document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById("login-form");
    const errorMessage = document.getElementById("error-message");

    loginForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        errorMessage.textContent = "";

        const identifier = document.getElementById("identifier").value;
        const password = document.getElementById("password").value;
        const deviceId = document.getElementById("deviceId").value;

        try {
            const response = await fetch("/api/v1/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ identifier, password, deviceId })
            });

            const data = await response.json();

            if (response.ok) {
                // Вход успешен, сохраняем токен и переходим в админку
                localStorage.setItem("adminToken", data.accessToken);
                window.location.href = "/index.html"; // или просто "/"
            } else {
                // Показываем ошибку (из GlobalExceptionHandlerMiddleware)
                errorMessage.textContent = data.message || "Ошибка входа";
            }
        } catch (error) {
            console.error("Login error:", error);
            errorMessage.textContent = "Не удалось подключиться к серверу.";
        }
    });
});