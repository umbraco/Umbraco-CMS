import type { UmbPropertyEditorUIMediaPickerElement } from './property-editor-ui-media-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-media-picker.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Media Picker',
	component: 'umb-property-editor-ui-media-picker',
	id: 'umb-property-editor-ui-media-picker',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIMediaPickerElement> = () =>
	html`<umb-property-editor-ui-media-picker></umb-property-editor-ui-media-picker>`;
