import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import type {
	UmbPropertyEditorUIPickerModalElement,
	UmbModalPropertyEditorUIPickerData,
} from './property-editor-ui-picker-modal.element';
import './property-editor-ui-picker-modal.element';

import '../../../components/body-layout/body-layout.element';

export default {
	title: 'API/Modals/Layouts/Property Editor UI Picker',
	component: 'umb-modal-layout-property-editor-ui-picker',
	id: 'modal-layout-property-editor-ui-picker',
} as Meta;

const data: UmbModalPropertyEditorUIPickerData = { selection: [] };

export const Overview: Story<UmbPropertyEditorUIPickerModalElement> = () => html`
	<umb-modal-layout-property-editor-ui-picker .data=${data as any}></umb-modal-layout-property-editor-ui-picker>
`;
