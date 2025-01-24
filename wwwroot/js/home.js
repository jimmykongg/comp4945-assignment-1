/**
 * The Main Work flow of home page.
 * Getting categories from backend and displaying them in cards
 * For Admin user, shows the Add Category button
 */
document.addEventListener("DOMContentLoaded", async function () {
    const token = localStorage.getItem("authToken");
    const username = localStorage.getItem("username");
    const role = localStorage.getItem("role");

    if (!token) {
        alert("You must log in to access this page.");
        window.location.href = "login.html";
        return;
    }

    document.getElementById("welcome-message").innerText = `Welcome, ${username} (${role})!`;
    document.getElementById("lobby-btn").addEventListener("click", async () => {
        window.location.href = "lobby.html";
    })

    // Show "Create New Category" button for admin users
    if (role === "admin") {
        setupCreateCategoryButton();
    }

    try {
        // Fetch and display categories
        await fetchCategories(token, role);
    } catch (error) {
        console.error("Error:", error);
        alert("Error loading categories. Please try again.");
    }

    // Logout functionality
    setupLogoutButton();
});


/**
 * Helper Functions
 */
function setupCreateCategoryButton() {
    const createCategoryBtn = document.getElementById("create-category-btn");
    createCategoryBtn.style.display = "block";  // Show the button for admins

    createCategoryBtn.addEventListener("click", () => {
        const categoryName = prompt("Enter the name of the new category:");
        if (!categoryName || categoryName.trim() === "") {
            alert("Category name cannot be empty.");
            return;
        }

        createCategory(categoryName.trim());
    });
}

async function createCategory(categoryName) {
    const token = localStorage.getItem("authToken");
    try {
        const response = await fetch("/api/categories", {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ name: categoryName })
        });

        if (response.ok) {
            alert("Category created successfully!");
            location.reload();  // Reload the page to show the new category
        } else {
            const error = await response.text();
            alert(`Error: ${error}`);
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Failed to create category. Please try again.");
    }
}

async function fetchCategories(token, role) {
    const response = await fetch("/api/categories", {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${token}`,
            "Content-Type": "application/json"
        }
    });

    if (!response.ok) {
        throw new Error("Failed to fetch categories.");
    }

    const categories = await response.json();  // Array of categories [{ id, name }, ...]
    const categoryList = document.getElementById("category-list");
    categoryList.innerHTML = "";  // Clear the list

    categories.forEach(category => {
        const card = createCategoryCard(category, role);
        categoryList.appendChild(card);
    });
}

function createCategoryCard(category, role) {
    const card = document.createElement("div");
    card.classList.add("category-card");

    card.innerHTML = `
    <h3>${category.name}</h3>
    <div class="button-group">
        <button class="play-btn" onclick="playQuiz(${category.id})">Play</button>
        <button class="autoplay-btn" onclick="autoplayQuiz(${category.id})">Autoplay</button>
        ${role === "admin" ? `
            <button class="edit-btn" onclick="editCategory(${category.id})">Edit</button>
            <button class="quizroom-btn" onclick="createQuizRoom(${category.id})">Quiz Room</button>
        ` : ''}
    </div>
`;
    return card;
}

function setupLogoutButton() {
    const logoutBtn = document.getElementById("logout");
    logoutBtn.addEventListener("click", () => {
        localStorage.clear();
        window.location.href = "login.html";
    });
}

// Placeholder functions for quiz actions
function playQuiz(categoryId) {
    // Redirect to quiz.html with autoplay set to false
    window.location.href = `quiz.html?categoryID=${categoryId}&autoplay=false`;
}

function autoplayQuiz(categoryId) {
    // Redirect to quiz.html with autoplay set to true
    window.location.href = `quiz.html?categoryID=${categoryId}&autoplay=true`;
}

function editCategory(categoryId) {
    window.location.href = `edit.html?categoryID=${categoryId}`;
}

function createQuizRoom(categoryId) {
    window.location.href = `quizRoomAdmin.html?categoryID=${categoryId}`;
}
