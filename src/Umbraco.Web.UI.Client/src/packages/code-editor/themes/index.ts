import type { CodeEditorTheme } from '../models/code-editor.model.js';
import { UmbCodeEditorThemeHighContrastLight } from './code-editor.hc-light.theme.js';
import { UmbCodeEditorThemeHighContrastDark } from './code-editor.hc-dark.theme.js';
import { UmbCodeEditorThemeLight } from './code-editor.light.theme.js';
import { UmbCodeEditorThemeDark } from './code-editor.dark.theme.js';
import type { monaco } from '@umbraco-cms/backoffice/external/monaco-editor';

/**
 * 4 themes for the code editor.
  @type {*} */
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
