import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import { UmbModalLayoutContentPickerElement, UmbModalContentPickerData } from './modal-layout-content-picker.element';
import './modal-layout-content-picker.element';

import '../../../../../backoffice/components/editor-layout.element';

export default {
	title: 'API/Modals/Layouts/Content Picker',
	component: 'umb-modal-layout-content-picker',
	id: 'modal-layout-content-picker',
} as Meta;

const data: UmbModalContentPickerData = {};

export const Overview: Story<UmbModalLayoutConfirmElement> = () => html`
	<umb-modal-layout-content-picker .data=${data}></umb-modal-layout-content-picker>
`;
