"use strict";
export class NotFollowed {
	constructor() {
		this.setEventListeners();
	}

	setEventListeners() {
		/**
		 * Event listener that triggers when the DOM content has been fully loaded.
		 * This function fetches a list of not followed users from the API and updates the UI with suggested users.
		 */
		document.addEventListener("DOMContentLoaded", () => {
			// Select the input element for the anti-forgery token
			const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');

			// Proceed if the anti-forgery token input is found
			if (tokenInput) {
				const token = tokenInput.value;

				// Fetch the list of not followed users from the endpoint
				fetch("/api/follows/notfollowed")
					.then((response) => response.json()) // Parse the response as JSON
					.then((data) => {
						// Update the UI with suggested users using the fetched data and token
						this.updateUiWithListOfSuggestedUsers(data, token);
					})
					.catch((error) => {
						// Log and handle the error if fetching data fails
						console.error("Error:", error);
					});
			}
		});
	}

	/**
	 * Creates a follow form for a user with the provided information.
	 * @param {Object} user - The user object containing user information.
	 * @param {string} token - The anti-forgery token used for form submissions.
	 * @returns {HTMLElement} The created form element for following the user.
	 */
	createFollowForm(user, token) {
		const form = document.createElement("form");
		form.classList.add("card-body");
		form.setAttribute("action", "/Follows/Create");
		form.setAttribute("method", "post");

		// Add the anti-forgery token as a hidden input
		const tokenInput = document.createElement("input");
		tokenInput.setAttribute("type", "hidden");
		tokenInput.setAttribute("name", "__RequestVerificationToken");
		tokenInput.setAttribute("value", token);

		const h5 = document.createElement("h5");
		h5.classList.add("card-title", "d-inline");
		h5.innerText = user.userName;

		const button = document.createElement("button");
		button.classList.add("btn", "btn-primary--dark", "d-inline", "rounded-pill", "float-end");
		button.innerText = "Follow";
		button.setAttribute("type", "submit");

		const userIdInput = document.createElement("input");
		userIdInput.setAttribute("type", "hidden");
		userIdInput.setAttribute("name", "userId");
		userIdInput.setAttribute("value", user.id);

		form.appendChild(tokenInput);
		form.appendChild(h5);
		form.appendChild(userIdInput);
		form.appendChild(button);

		return form;
	}

	/**
	 * Updates the user interface by displaying a list of suggested users and creating follow forms.
	 * @param {Array} users - An array of user objects representing suggested users.
	 * @param {string} token - The anti-forgery token used for form submissions.
	 */
	updateUiWithListOfSuggestedUsers(users, token) {
		const suggestedUsersCard = document.getElementById("suggestedUsersCard");
		const suggestedUsers = document.getElementById("suggestedUsers");
		// Clear existing content in the suggested users card
		if (suggestedUsers && suggestedUsersCard) {
			suggestedUsers.innerHTML = "";
			// Display suggested users and create follow forms if there are users
			if (users.length > 0) {
				// Remove the "d-none" class to make the suggested users card visible
				suggestedUsersCard.classList.remove("d-none");

				// Create follow forms for each suggested user and append them to the card
				users.forEach((user) => {
					const form = this.createFollowForm(user, token);
					suggestedUsersCard.appendChild(form);
				});
			}
		}
	}
}

const notFollowed = new NotFollowed();
