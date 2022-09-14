import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import type {
	UmbModalLayoutPropertyEditorPickerElement,
	UmbModalPropertyEditorPickerData,
} from './modal-layout-property-editor-picker.element';
import './modal-layout-property-editor-picker.element';

import '../../../../../backoffice/editors/shared/editor-layout/editor-layout.element';

export default {
	title: 'API/Modals/Layouts/Property Editor Picker',
	component: 'umb-modal-layout-property-editor-picker',
	id: 'modal-layout-property-editor-picker',
} as Meta;

const data: UmbModalPropertyEditorPickerData = { selection: [] };

export const Overview: Story<UmbModalLayoutPropertyEditorPickerElement> = () => html`
	<umb-modal-layout-property-editor-picker .data=${data as any}></umb-modal-layout-property-editor-picker>
`;
