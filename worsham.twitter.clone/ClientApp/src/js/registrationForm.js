﻿import * as bootstrap from "bootstrap";

export class RegistrationForm {
	constructor() {
		this.initializeForm();
	}

	initializeForm() {
		$(document).ready(function () {
			$("#createAccountForm").submit(function (event) {
				event.preventDefault(); // Prevent the default form submission

				const form = $(this); // Now 'this' refers to the form element
				const formData = form.serialize();

				$.ajax({
					type: "POST",
					url: form.attr("action"),
					data: formData,
					dataType: "json",
					success: function (result) {
						if (result.success) {
							window.location.href = "/Home/DisplaySignInModal";
						} else {
							// Update the error message element in the _CreateAccountModal
							$("#createAccountErrorMessage").text(result.errorMessage);
						}
					},
					error: function (xhr) {
						let errors;
						switch (xhr.status) {
							case 400:
								errors = xhr.responseJSON.errors;
								$("#createAccountErrorMessage").text(errors.join(", "));
								break;
							case 401:
								$("#createAccountErrorMessage").text("You are not authorized to perform this action.");
								break;
							case 500:
								$("#createAccountErrorMessage").text("An internal server error occurred. Please try again later.");
								break;
							default:
								$("#createAccountErrorMessage").text("An error occurred. Please try again later.");
						}
					},
				});
			});

			const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
			if (tooltipTriggerList) {
				const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
					return new bootstrap.Tooltip(tooltipTriggerEl);
				});
			}
		});
	}
}

const registrationForm = new RegistrationForm();