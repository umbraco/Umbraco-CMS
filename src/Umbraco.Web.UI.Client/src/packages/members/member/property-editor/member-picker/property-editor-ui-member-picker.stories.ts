import type { UmbPropertyEditorUIMemberPickerElement } from './property-editor-ui-member-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-member-picker.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Member Picker',
	component: 'umb-property-editor-ui-member-picker',
	id: 'umb-property-editor-ui-member-picker',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIMemberPickerElement> = () =>
	html`<umb-property-editor-ui-member-picker></umb-property-editor-ui-member-picker>`;
