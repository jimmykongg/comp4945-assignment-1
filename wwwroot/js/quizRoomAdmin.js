document.addEventListener("DOMContentLoaded", async function () {
    const params = new URLSearchParams(window.location.search);
    const categoryId = params.get("categoryID");
    const roomId = params.get("roomID");
    const username = localStorage.getItem("username");
    const token = localStorage.getItem("authToken");

    if (!username || !token) {
        alert("Missing required parameters or you are not logged in.");
        window.location.href = "lobby.html"; // Redirect to lobby if invalid
        return;
    }

    // Admin-specific UI elements
    const participantList = document.getElementById("participant-list");
    const quizDescription = document.getElementById("quiz-description");
    const quizMedia = document.getElementById("quiz-media");
    const mediaImage = document.getElementById("media-image");
    const mediaVideo = document.getElementById("media-video");
    const videoSource = document.getElementById("video-source");
    const quizAnswers = document.getElementById("quiz-answers");

    let socket;

    // Admin creates a new room
    console.log("Admin Creating a Room");
    socket = new WebSocket(`ws://localhost:5276/api/quizroom/create/${categoryId}?username=${username}`);

    // WebSocket event handlers
    socket.onopen = () => {
        console.log("WebSocket connection established.");
    };

    socket.onmessage = (event) => {
        const message = JSON.parse(event.data);

        switch (message.type) {
            case "question":
                displayQuestion(message.data);
                break;
            case "newParticipant":
                updateParticipantList(message.data.username);
                break;
            case "answerSelected":
                notifyAnswerSelection(message.data.selectedAnswerId);
                break;
            case "endQuiz":
                alert("Quiz has ended.");
                socket.close();
                window.location.href = "home.html";
                break;
            default:
                console.log("Unknown message type:", message);
        }
    };

    socket.onerror = (error) => {
        console.error("WebSocket error:", error);
        alert("An error occurred with the WebSocket connection.");
    };

    socket.onclose = () => {
        console.log("WebSocket connection closed.");
        alert("WebSocket connection closed.");
    };

    // Admin-specific functionality
    document.getElementById("reveal-answer-btn").addEventListener("click", () => {
        const correctAnswerDiv = document.querySelector(".answer-item[data-correct='true']");
        if (correctAnswerDiv) {
            correctAnswerDiv.style.backgroundColor = "#4caf50"; // Highlight in green
            correctAnswerDiv.style.color = "white"; // Optional: change text color for better visibility
        } else {
            console.log("No correct answer found!");
        }
    });

    document.getElementById("next-question-btn").addEventListener("click", () => {
        socket.send(JSON.stringify({ type: "next" }));
    });

    // Helper Functions
    function displayQuestion(question) {
        console.log(question);
        quizDescription.textContent = question.Description;

        if (question.Media) {
            quizMedia.style.display = "block";
            if (question.Media.MediaType === "image") {
                mediaImage.src = `..${question.Media.FilePath}`;
                mediaImage.style.display = "block";
                mediaVideo.style.display = "none";
            } else if (question.Media.MediaType === "video") {
                const iframe = document.createElement("iframe");
                iframe.src = `https://www.youtube.com/embed/${question.Media.FilePath}?autoplay=1&mute=1&enablejsapi=1`;
                iframe.width = "560";
                iframe.height = "315";
                iframe.title = "YouTube video player";
                iframe.frameBorder = "0";
                iframe.allow = "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture";
                iframe.allowFullscreen = true;
                mediaVideo.style.display = "block";
                mediaVideo.appendChild(iframe);
                mediaImage.style.display = "none";
            }
        } else {
            quizMedia.style.display = "none";
        }

        quizAnswers.innerHTML = ""; // Clear old answers
        question.Answers.forEach((answer) => {
            const answerDiv = document.createElement("div");
            answerDiv.className = "answer-item";
            answerDiv.dataset.answerId = answer.Id;
            answerDiv.dataset.correct = answer.RightAnswer;
            answerDiv.innerHTML = `
                <span>${answer.Description}</span>
                <span class="answer-count">0</span>
            `;
            quizAnswers.appendChild(answerDiv);
        });
    }

    function updateParticipantList(participantName) {
        const participantItem = document.createElement("li");
        participantItem.textContent = participantName;
        participantList.appendChild(participantItem);
    }

    function notifyAnswerSelection(selectedAnswerId) {
        const answerDiv = document.querySelector(`.answer-item[data-answer-id="${selectedAnswerId}"]`);
        if (answerDiv) {
            const countSpan = answerDiv.querySelector(".answer-count");
            const currentCount = parseInt(countSpan.textContent, 10);
            countSpan.textContent = currentCount + 1; // Increment the count
        }
    }
});
