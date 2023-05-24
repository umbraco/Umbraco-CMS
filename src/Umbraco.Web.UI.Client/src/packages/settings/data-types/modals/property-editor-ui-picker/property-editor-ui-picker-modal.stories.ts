import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUIPickerModalElement } from './property-editor-ui-picker-modal.element.js';
import type { UmbPropertyEditorUIPickerModalData } from '@umbraco-cms/backoffice/modal';

import './property-editor-ui-picker-modal.element';
import '../../../../core/components/body-layout/body-layout.element';

export default {
	title: 'API/Modals/Layouts/Property Editor UI Picker',
	component: 'umb-property-editor-ui-picker-modal',
	id: 'umb-property-editor-ui-picker-modal',
} as Meta;

const data: UmbPropertyEditorUIPickerModalData = { selection: [] };

export const Overview: Story<UmbPropertyEditorUIPickerModalElement> = () => html`
	<umb-property-editor-ui-picker-modal .data=${data as any}></umb-property-editor-ui-picker-modal>
`;
