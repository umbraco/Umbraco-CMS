// TODO: we can try and make pretty urls if we want to
export const urlFriendlyPathFromServerFilePath = (path: string) => encodeURIComponent(path).replace('.', '-');

// TODO: we can try and make pretty urls if we want to
export const serverFilePathFromUrlFriendlyPath = (unique: string) => decodeURIComponent(unique.replace('-', '.'));
