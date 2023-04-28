function saveAsFileJson(filename, data) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + data;
    link.click();

}