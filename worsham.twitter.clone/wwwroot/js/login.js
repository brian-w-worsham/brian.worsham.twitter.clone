$(document).ready(function () {
    $("#loginForm").submit(function (event) {
        event.preventDefault(); // Prevent the default form submission

        const form = $(this);
        const formData = form.serialize(); // Serialize form data

        $.ajax({
            type: "POST",
            url: form.attr("action"),
            data: formData,
            dataType: "json",
            success: function (result) {
                if (result.success) {
                    // Redirect or perform any other actions upon successful login
                    window.location.href = "/Tweets/Index";
                } else {
                    // Update the error message element
                    $("#loginErrorMessage").text(result.errorMessage);
                }
            },
            error: function (xhr) {
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
    });
});
