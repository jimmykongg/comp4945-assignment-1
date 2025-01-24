document.addEventListener("DOMContentLoaded", async function () {
    const params = new URLSearchParams(window.location.search);
    const roomId = params.get("roomId");
    const username = localStorage.getItem("username");
    const token = localStorage.getItem("authToken");

    if (!username || !token || !roomId) {
        alert("Missing required parameters or you are not logged in.");
        window.location.href = "lobby.html"; // Redirect to lobby if invalid
        return;
    }

    const quizDescription = document.getElementById("quiz-description");
    const quizMedia = document.getElementById("quiz-media");
    const mediaImage = document.getElementById("media-image");
    const mediaVideo = document.getElementById("media-video");
    const videoSource = document.getElementById("video-source");
    const quizAnswers = document.getElementById("quiz-answers");
    const submitBtn = document.getElementById("submit-btn");

    let selectedAnswerId = null;

    // Create WebSocket connection
    const socket = new WebSocket(`ws://localhost:5276/api/quizroom/join/${roomId}?username=${username}`);

    // WebSocket event handlers
    socket.onopen = () => {
        console.log("WebSocket connection established.");
    };

    socket.onmessage = (event) => {
        const message = JSON.parse(event.data);

        console.log("Participants received a message: " + message);
        switch (message.type) {
            case "question":
                console.log("Question received:", message.data);
                displayQuestion(message.data);
                break;
            case "endQuiz":
                alert("The quiz has ended. Thank you for participating!");
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
    };

    // Display the question and answers
    function displayQuestion(question) {
        console.log("Populating the page!")
        quizDescription.textContent = question.Description;
        console.log("Quiz description added" + quizDescription.textContent);
        // Display media if available
        
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

        // Populate answers
        quizAnswers.innerHTML = ""; // Clear old answers
        question.Answers.forEach((answer) => {
            const answerDiv = document.createElement("div");
            answerDiv.className = "answer-item";
            answerDiv.dataset.answerId = answer.Id;
            answerDiv.textContent = answer.Description;

            // Highlight the selected answer
            answerDiv.addEventListener("click", () => {
                document.querySelectorAll(".answer-item").forEach(item => item.classList.remove("selected"));
                answerDiv.classList.add("selected");
                selectedAnswerId = answer.Id; // Store the selected answer ID
                submitBtn.disabled = false; // Enable submit button
            });

            quizAnswers.appendChild(answerDiv);
            console.log("new div added" + answerDiv.textContent);
            
        });

        // Disable submit button until an answer is selected
        submitBtn.disabled = true;
    }

    // Submit the selected answer
    submitBtn.addEventListener("click", () => {
        if (selectedAnswerId) {
            const message = {
                type: "answerSelected",
                selectedAnswerId: selectedAnswerId
            };
            socket.send(JSON.stringify(message));
            alert("Your answer has been submitted.");
            submitBtn.disabled = true; // Disable submit button to prevent resubmission
        } else {
            alert("Please select an answer before submitting.");
        }
    });
});
