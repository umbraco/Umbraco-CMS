import type { UmbPropertyEditorUIMemberGroupPickerElement } from './property-editor-ui-member-group-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-member-group-picker.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Member Group Picker',
	component: 'umb-property-editor-ui-member-group-picker',
	id: 'umb-property-editor-ui-member-group-picker',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIMemberGroupPickerElement> = () =>
	html`<umb-property-editor-ui-member-group-picker></umb-property-editor-ui-member-group-picker>`;
