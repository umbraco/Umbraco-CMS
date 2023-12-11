import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUIPickerModalElement } from './property-editor-ui-picker-modal.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUIPickerModalValue } from '@umbraco-cms/backoffice/modal';

import './property-editor-ui-picker-modal.element.js';
import '../../../components/body-layout/body-layout.element.js';

export default {
	title: 'API/Modals/Layouts/Property Editor UI Picker',
	component: 'umb-property-editor-ui-picker-modal',
	id: 'umb-property-editor-ui-picker-modal',
} as Meta;

const data: UmbPropertyEditorUIPickerModalValue = { selection: [] };

export const Overview: Story<UmbPropertyEditorUIPickerModalElement> = () => html`
	<umb-property-editor-ui-picker-modal .value=${data as any}></umb-property-editor-ui-picker-modal>
`;
