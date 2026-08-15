window.copyToClipboard = function (text) {
    return new Promise(function (resolve, reject) {
        if (navigator.clipboard && window.isSecureContext) {
            navigator.clipboard.writeText(text).then(resolve).catch(function () {
                fallbackCopy(text, resolve, reject);
            });
        } else {
            fallbackCopy(text, resolve, reject);
        }
    });
};

function fallbackCopy(text, resolve, reject) {
    var textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.setAttribute('readonly', '');
    textarea.style.position = 'fixed';
    textarea.style.top = '0';
    textarea.style.opacity = '0';
    document.body.appendChild(textarea);
    textarea.focus();
    textarea.select();
    try {
        document.execCommand('copy');
        resolve();
    } catch (err) {
        reject(err);
    }
    document.body.removeChild(textarea);
}