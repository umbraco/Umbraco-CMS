import * as monaco from 'monaco-editor';
import { CodeEditorTheme } from '../code-editor.model.js';
import { UmbCodeEditorThemeHighContrastLight } from './code-editor.hc-light.theme.js';
import { UmbCodeEditorThemeHighContrastDark } from './code-editor.hc-dark.theme.js';
import { UmbCodeEditorThemeLight } from './code-editor.light.theme.js';
import { UmbCodeEditorThemeDark } from './code-editor.dark.theme.js';
/**
 * 4 themes for the code editor.
 *
 * @type {*} */
const themes: Record<CodeEditorTheme, monaco.editor.IStandaloneThemeData> = {
	'umb-dark': UmbCodeEditorThemeDark,
	'umb-light': UmbCodeEditorThemeLight,
	'umb-hc-light': UmbCodeEditorThemeHighContrastLight,
	'umb-hc-dark': UmbCodeEditorThemeHighContrastDark,
};
export {
	UmbCodeEditorThemeDark,
	UmbCodeEditorThemeLight,
	UmbCodeEditorThemeHighContrastLight,
	UmbCodeEditorThemeHighContrastDark,
	themes,
};
export default themes;
