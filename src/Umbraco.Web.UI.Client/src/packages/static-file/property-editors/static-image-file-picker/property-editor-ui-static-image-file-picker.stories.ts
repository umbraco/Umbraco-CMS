import type { UmbPropertyEditorUIStaticImageFilePickerElement } from './property-editor-ui-static-image-file-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-static-image-file-picker.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Static Image File Picker',
	component: 'umb-property-editor-ui-static-image-file-picker',
	id: 'umb-property-editor-ui-static-image-file-picker',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIStaticImageFilePickerElement> = () =>
	html` <umb-property-editor-ui-static-image-file-picker></umb-property-editor-ui-static-image-file-picker>`;
