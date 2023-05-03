import * as monaco from 'monaco-editor';
import { CodeEditorTheme } from '../code-editor.model';
import { UmbCodeEditorThemeHighContrastLight } from './code-editor.hc-light.theme';
import { UmbCodeEditorThemeHighContrastDark } from './code-editor.hc-dark.theme';
import { UmbCodeEditorThemeLight } from './code-editor.light.theme';
import { UmbCodeEditorThemeDark } from './code-editor.dark.theme';
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
