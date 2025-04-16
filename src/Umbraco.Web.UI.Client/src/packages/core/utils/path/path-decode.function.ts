// Notice, no need to handle . or : specifically as decodeURIComponent does handle these. [NL]
export const decodeFilePath = (path: string) => decodeURIComponent(path);
