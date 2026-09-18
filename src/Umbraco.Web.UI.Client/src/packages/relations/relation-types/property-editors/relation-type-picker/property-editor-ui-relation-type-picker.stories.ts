import type { UmbPropertyEditorUIRelationTypePickerElement } from './property-editor-ui-relation-type-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-relation-type-picker.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Relation Type Picker',
	component: 'umb-property-editor-ui-relation-type-picker',
	id: 'umb-property-editor-ui-relation-type-picker',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIRelationTypePickerElement> = () =>
	html` <umb-property-editor-ui-relation-type-picker></umb-property-editor-ui-relation-type-picker>`;
