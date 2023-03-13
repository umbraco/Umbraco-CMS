import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import type { UmbPropertyEditorUIPickerModalElement } from './property-editor-ui-picker-modal.element';
import type { UmbPropertyEditorUIPickerModalData } from './';

import './property-editor-ui-picker-modal.element';
import '../../../components/body-layout/body-layout.element';

export default {
	title: 'API/Modals/Layouts/Property Editor UI Picker',
	component: 'umb-property-editor-ui-picker-modal',
	id: 'umb-property-editor-ui-picker-modal',
} as Meta;

const data: UmbPropertyEditorUIPickerModalData = { selection: [] };

export const Overview: Story<UmbPropertyEditorUIPickerModalElement> = () => html`
	<umb-property-editor-ui-picker-modal .data=${data as any}></umb-property-editor-ui-picker-modal>
`;
