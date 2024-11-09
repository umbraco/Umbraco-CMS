import type { UmbPropertyEditorUIPickerModalElement } from './property-editor-ui-picker-modal.element.js';
import type { UmbPropertyEditorUIPickerModalValue } from './property-editor-ui-picker-modal.token.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-picker-modal.element.js';
import '../../components/body-layout/body-layout.element.js';

export default {
	title: 'API/Modals/Layouts/Property Editor UI Picker',
	component: 'umb-property-editor-ui-picker-modal',
	id: 'umb-property-editor-ui-picker-modal',
} as Meta;

const data: UmbPropertyEditorUIPickerModalValue = { selection: [] };

export const Overview: StoryFn<UmbPropertyEditorUIPickerModalElement> = () => html`
	<umb-property-editor-ui-picker-modal .value=${data as any}></umb-property-editor-ui-picker-modal>
`;
