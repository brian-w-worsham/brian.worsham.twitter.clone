import $ from "jquery";
export class Login {
	constructor() {
		this.initializeLoginForm();
	}

	initializeLoginForm() {
		$(document).ready(() => {
			const loginForm = $("#loginForm");

			loginForm.submit((event) => {
				event.preventDefault(); // Prevent the default form submission

				const formData = {
					UserName: loginForm.find('[name="UserName"]').val(),
					Password: loginForm.find('[name="Password"]').val(),
				};

				this.sendLoginRequest(loginForm, formData);
			});
		});
	}

	sendLoginRequest(form, formData) {
		$.ajax({
			type: "POST",
			url: form.attr("action"),
			data: JSON.stringify(formData), // Convert to JSON string
			contentType: "application/json", // Set the content type to JSON
			dataType: "json",
			success: (result) => {
				if (result.success) {
					// Redirect or perform any other actions upon successful login
					window.location.href = "/Tweets/Index";
				} else {
					// Update the error message element
					$("#loginErrorMessage").text(result.errorMessage);
				}
			},
			error: (xhr) => {
				let errors;
				switch (xhr.status) {
					case 400:
						errors = xhr.responseJSON.errors;
						$("#loginErrorMessage").text(errors.join(", "));
						break;
					case 401:
						$("#loginErrorMessage").text("Invalid credentials. Please check your username and password.");
						break;
					case 500:
						$("#loginErrorMessage").text("An internal server error occurred. Please try again later.");
						break;
					default:
						$("#loginErrorMessage").text("An error occurred. Please try again later.");
				}
			},
		});
	}
}

const login = new Login();