import { readFileSync } from 'fs';

export const packageJsonPath = 'package.json';
export const packageJsonData = JSON.parse(readFileSync(packageJsonPath).toString());
export const packageJsonName = packageJsonData.name;
export const packageJsonVersion = packageJsonData.version;
export const packageJsonExports = packageJsonData.exports;
