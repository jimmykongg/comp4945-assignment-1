
// Select the signup form and add an event listener
const signupForm = document.getElementById("signup-form");
if (signupForm) {
    console.log("Adding event for signup form")
    signupForm.addEventListener("submit", async function (event) {
        event.preventDefault();  // Prevent the default form submission

        // Get form values
        const username = document.getElementById("username").value;
        const password = document.getElementById("password").value;
        const confirmPassword = document.getElementById("confirm-password").value;
        const role = document.getElementById("role").value;

        // Check if passwords match
        if (password !== confirmPassword) {
            alert("Passwords do not match.");
            return;
        }

        // Create request payload
        const requestBody = {
            username: username,
            password: password,
            role: role
        };

        try {
            // Send POST request to the backend
            const response = await fetch("/api/auth/signup", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(requestBody)
            });

            if (response.ok) {
                const data = await response.json();
                alert("Signup successfully.");
                window.location.href = "login.html";  // Redirect to login page
            } else {
                const error = await response.text();
                alert(`Error: ${error}`);
            }
        } catch (error) {
            alert(`Failed to connect: ${error}`);
        }
    });
}

// Select the login form
const loginForm = document.getElementById("login-form");
if (loginForm) {
    console.log("Adding Event for Login form");
    loginForm.addEventListener("submit", async function (event) {
        event.preventDefault();

        const username = document.getElementById("username").value;
        const password = document.getElementById("password").value;

        const requestBody = { username, password };

        try {
            const response = await fetch("/api/auth/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(requestBody)
            });

            if (response.ok) {
                const data = await response.json();  // Parse response
                console.log("Response Data:", data);
                const token = data.token;  // JWT token from server
                const username = data.username;  // Username
                const role = data.role;    // User role (e.g., "admin" or "general")
                

                // Store user information in localStorage
                localStorage.setItem("authToken", token);  // JWT token
                localStorage.setItem("username", username);  // Store username
                localStorage.setItem("role", role);  // Store role (for conditional rendering)

                alert(`Welcome, ${username}!`);
                window.location.href = "home.html";  // Redirect to home page
            } else {
                const error = await response.text();
                alert(`Error: ${error}`);  // Show error message
            }
        } catch (error) {
            alert(`Failed to connect: ${error}`);  // Handle network errors
        }
    });
}
