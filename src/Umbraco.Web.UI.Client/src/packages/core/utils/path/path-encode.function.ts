export const encodeFilePath = (path: string) => encodeURIComponent(path).replaceAll('.', '%2E').replaceAll(':', '%3A');

export const aliasToPath = (path: string) => encodeFilePath(path);
