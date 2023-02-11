window.setImage = async (input, image) => {
    const url = URL.createObjectURL(input.files[0]);
    image.src = url;
    
    return url;
}

window.inputClick = async element => element.click();

window.disposeObjectURL = async url => URL.revokeObjectURL(url);