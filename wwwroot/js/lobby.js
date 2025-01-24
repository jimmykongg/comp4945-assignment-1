document.addEventListener("DOMContentLoaded", async () => {
    const token = localStorage.getItem("authToken");

    if (!token) {
        alert("You must log in to access this page.");
        window.location.href = "login.html";
        return;
    }

    const roomList = document.getElementById("room-list");
    const statusMessage = document.getElementById("status-message");

    document.getElementById("back-to-home").addEventListener("click", () => {
        window.location.href = "home.html";
    });

    try {
        const response = await fetch("/api/quizroom/list", {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        });

        if (response.ok) {
            const data = await response.json();

            if (Array.isArray(data) && data.length > 0) {
                statusMessage.textContent = "Available Quiz Rooms:";
                data.forEach(room => {
                    const listItem = document.createElement("li");
                    listItem.innerHTML = `
                        (Hosted by: ${room.host}) 
                        - Participants: ${room.participants}
                        <button class="join-room-btn" data-room-id="${room.roomId}">Join</button>
                    `;
                    roomList.appendChild(listItem);
                });

                // Attach event listeners to join buttons
                document.querySelectorAll(".join-room-btn").forEach(button => {
                    button.addEventListener("click", () => {
                        const roomId = button.getAttribute("data-room-id");
                        const username = localStorage.getItem("username"); // Retrieve username from local storage
                        if (!username) {
                            alert("You must log in to join a room.");
                            return;
                        }
                        window.location.href = `quizRoomUser.html?roomId=${roomId}&username=${username}`;
                    });
                });
            } else {
                statusMessage.textContent = data.message || "No available quiz rooms at the moment.";
            }
        } else {
            throw new Error(await response.text());
        }
    } catch (error) {
        console.error("Error:", error);
        statusMessage.textContent = "Failed to load quiz rooms. Please try again later.";
    }
});


