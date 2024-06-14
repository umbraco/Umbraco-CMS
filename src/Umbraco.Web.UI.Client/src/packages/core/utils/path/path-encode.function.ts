export const encodeFilePath = (path: string) => encodeURIComponent(path).replaceAll('.', '-');

export const aliasToPath = (path: string) => encodeFilePath(path);
