// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// โค้ดนี้อยู่ในไฟล์ wwwroot/js/site.js

// โค้ดนี้จะทำงานหลังจาก DOM ถูกโหลดแล้ว และมั่นใจว่า jQuery ถูกโหลดแล้ว (ตามลำดับใน Layout)
$(document).ready(function () { 

    // ใช้ Event Delegation บน 'document' เพื่อผูก Event คลิกกับปุ่ม 
    // .save-single-xray-btn ที่มาจาก View ที่โหลดแบบ Dynamic
    $(document).on('click', '.save-single-xray-btn', function (e) {
        e.preventDefault(); // ป้องกันการ Submit Form หากปุ่มอยู่ใน Form

        var $btn = $(this);
        var requestNo = $btn.data('requestno');
        var patientHn = $btn.data('hn');

        // สำคัญ: URL ต้องเป็น Hardcode/Static เพราะไม่สามารถใช้ @Url.Action ในไฟล์ JS ภายนอกได้
        var saveUrl = '/PatientCRA/SaveSingleXray'; 

        // ------------------
        // Validation Check
        // ------------------
        if (!requestNo || !patientHn) {
            console.error('Request Number or HN data is missing for the save action.');
            alert('Error: Data attributes are missing.');
            return;
        }

        var originalHtml = $btn.html();

        $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...');

        // ------------------
        // AJAX Call
        // ------------------
        $.ajax({
            url: saveUrl,
            type: 'POST',
            data: {
                requestNo: requestNo,
                hn: patientHn
            },
            success: function (response) {
                if (response.success) {
                    // Success feedback
                    alert('X-ray Record #' + requestNo + ' saved successfully!');
                    $btn.removeClass('btn-success').addClass('btn-secondary').html('<i class="bi bi-check-lg me-1"></i> Saved');
                } else {
                    // Server-side error feedback
                    alert('Error saving record #' + requestNo + ': ' + response.message);
                    $btn.prop('disabled', false).html(originalHtml);
                }
            },
            error: function (xhr, status, error) {
                // Network or client-side error feedback
                alert('An error occurred while saving to the server: ' + error);
                $btn.prop('disabled', false).html(originalHtml);
            }
        });
    });
});
