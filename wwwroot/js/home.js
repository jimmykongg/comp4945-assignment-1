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

    // Show "Create New Category" button for admin users
    if (role === "admin") {
        setupCreateCategoryButton();
    }

    try {
        // Fetch and display categories
        await fetchCategories(token, role);

        // Fetch and display rooms
        await fetchRooms(token);
    } catch (error) {
        console.error("Error:", error);
        alert("Error loading categories or rooms. Please try again.");
    }

    // Logout functionality
    setupLogoutButton();
});


/**
 * Helper Functions for Categories
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
            ${role === "admin" ? `<button class="edit-btn" onclick="editCategory(${category.id})">Edit</button>` : ''}
        </div>
    `;
    return card;
}


/**
 * Helper Functions for Rooms
 */
async function fetchRooms(token) {
    const response = await fetch("/api/quizrooms", {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${token}`,
            "Content-Type": "application/json"
        }
    });

    if (!response.ok) {
        throw new Error("Failed to fetch rooms.");
    }

    const rooms = await response.json();  // Array of rooms [{ id, name }, ...]
    const roomList = document.getElementById("room-list");
    roomList.innerHTML = "";  // Clear the list

    rooms.forEach(room => {
        const roomCard = createRoomCard(room);
        roomList.appendChild(roomCard);
    });
}

function createRoomCard(room) {
    const card = document.createElement("div");
    card.classList.add("room-card");

    card.innerHTML = `
        <h4>${room.name}</h4>
        <div class="button-group">
            <button class="join-btn" onclick="joinRoom(${room.id})">Join Room</button>
        </div>
    `;
    return card;
}

function setupCreateRoomButton() {
    const createRoomBtn = document.getElementById("create-room-btn");
    createRoomBtn.addEventListener("click", () => {
        const roomName = prompt("Enter the name of the new room:");
        if (!roomName || roomName.trim() === "") {
            alert("Room name cannot be empty.");
            return;
        }

        createRoom(roomName.trim());
    });
}

async function createRoom(roomName) {
    const token = localStorage.getItem("authToken");

    try {
        const response = await fetch("/api/quizrooms", {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ name: roomName })
        });

        if (response.ok) {
            alert("Room created successfully!");
            window.location.href = `create-room.html?roomName=${roomName}`;  // Redirect to create-room.html with room name
        } else {
            const error = await response.text();
            alert(`Error: ${error}`);
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Failed to create room. Please try again.");
    }
}

async function joinRoom(roomId) {
    const token = localStorage.getItem("authToken");

    try {
        const response = await fetch(`/api/quizrooms/${roomId}/join`, {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        });

        if (response.ok) {
            alert("Joined the room successfully!");
            window.location.href = `join-room.html?roomId=${roomId}`;  // Redirect to join-room.html with room ID
        } else {
            const error = await response.text();
            alert(`Error: ${error}`);
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Failed to join the room. Please try again.");
    }
}


/**
 * Logout functionality
 */
function setupLogoutButton() {
    const logoutBtn = document.getElementById("logout");
    logoutBtn.addEventListener("click", () => {
        localStorage.clear();
        window.location.href = "login.html";
    });
}

// Placeholder functions for quiz actions
function playQuiz(categoryId) {
    window.location.href = `quiz.html?categoryID=${categoryId}&autoplay=false`;
}

function autoplayQuiz(categoryId) {
    window.location.href = `quiz.html?categoryID=${categoryId}&autoplay=true`;
}

function editCategory(categoryId) {
    window.location.href = `edit.html?categoryID=${categoryId}`;
}
