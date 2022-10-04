import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import type {
	UmbModalLayoutPropertyEditorUIPickerElement,
	UmbModalPropertyEditorUIPickerData,
} from './modal-layout-property-editor-ui-picker.element';
import './modal-layout-property-editor-ui-picker.element';

import '../../../../../backoffice/editors/shared/editor-layout/editor-layout.element';

export default {
	title: 'API/Modals/Layouts/Property Editor UI Picker',
	component: 'umb-modal-layout-property-editor-ui-picker',
	id: 'modal-layout-property-editor-ui-picker',
} as Meta;

const data: UmbModalPropertyEditorUIPickerData = { selection: [] };

export const Overview: Story<UmbModalLayoutPropertyEditorUIPickerElement> = () => html`
	<umb-modal-layout-property-editor-ui-picker .data=${data as any}></umb-modal-layout-property-editor-ui-picker>
`;
