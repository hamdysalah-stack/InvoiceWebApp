window.InvoiceApp = window.InvoiceApp || {};

InvoiceApp.Notifications = {
    showSuccess: function (message, title = 'Success') {
        toastr.success(message, title);
    },

    showError: function (message, title = 'Error') {
        toastr.error(message, title);
    },

    showInfo: function (message, title = 'Information') {
        toastr.info(message, title);
    },

    showWarning: function (message, title = 'Warning') {
        toastr.warning(message, title);
    },

    confirmDelete: function (options = {}) {
        const defaultOptions = {
            title: 'Are you sure?',
            text: 'You won\'t be able to revert this!',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, delete it!',
            cancelButtonText: 'Cancel',
            reverseButtons: true,
            focusCancel: true
        };

        const finalOptions = { ...defaultOptions, ...options };

        return Swal.fire(finalOptions);
    },

    showSweetSuccess: function (title = 'Success!', text = '', timer = 2000) {
        Swal.fire({
            title: title,
            text: text,
            icon: 'success',
            timer: timer,
            showConfirmButton: false,
            timerProgressBar: true
        });
    },

    showSweetError: function (title = 'Error!', text = '') {
        Swal.fire({
            title: title,
            text: text,
            icon: 'error',
            confirmButtonColor: '#dc3545'
        });
    },

    showLoading: function (message = 'Processing...') {
        toastr.info(message, 'Please wait', {
            timeOut: 0,
            extendedTimeOut: 0,
            closeButton: false,
            tapToDismiss: false
        });
    },

    clearAll: function () {
        toastr.clear();
    }
};

$(document).ready(function () {
    $(document).on('click', '.btn-delete, .delete-btn', function (e) {
        e.preventDefault();

        const button = $(this);
        const form = button.closest('form');
        const itemName = button.data('item-name') || 'this item';
        const itemId = button.data('item-id') || '';

        InvoiceApp.Notifications.confirmDelete({
            title: 'Delete Confirmation',
            text: `Are you sure you want to delete ${itemName}?`,
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                InvoiceApp.Notifications.showLoading('Deleting...');

                if (form.length > 0) {
                    form.submit();
                } else {
                    const deleteUrl = button.data('delete-url') || button.attr('href');
                    if (deleteUrl) {
                        window.location.href = deleteUrl;
                    }
                }
            }
        });
    });

    $(document).on('submit', '.ajax-form', function (e) {
        e.preventDefault();

        const form = $(this);
        const submitBtn = form.find('button[type="submit"]');
        const originalText = submitBtn.html();

        submitBtn.html('<span class="loading"></span> Processing...').prop('disabled', true);

        $.ajax({
            url: form.attr('action'),
            type: form.attr('method') || 'POST',
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    InvoiceApp.Notifications.showSuccess(response.message || 'Operation completed successfully');

                    if (response.redirectUrl) {
                        setTimeout(() => {
                            window.location.href = response.redirectUrl;
                        }, 1000);
                    }
                } else {
                    InvoiceApp.Notifications.showError(response.message || 'An error occurred');
                }
            },
            error: function (xhr, status, error) {
                InvoiceApp.Notifications.showError('An error occurred while processing your request');
            },
            complete: function () {
                submitBtn.html(originalText).prop('disabled', false);
            }
        });
    });

    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 5000);
});
