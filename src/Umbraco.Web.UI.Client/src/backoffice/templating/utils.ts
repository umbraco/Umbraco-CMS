// TODO: we can try and make pretty urls if we want to
export const urlFriendlyPathFromServerPath = (path: string) => encodeURIComponent(path).replace('.', '-');

// TODO: we can try and make pretty urls if we want to
export const serverPathFromUrlFriendlyPath = (unique: string) => decodeURIComponent(unique.replace('-', '.'));
