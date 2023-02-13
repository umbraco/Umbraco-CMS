import '../../../../backoffice/shared/components/body-layout/body-layout.element';
import './modal-layout-multi-url-picker.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type {
	UmbModalLayoutMultiUrlPickerElement,
	UmbModalMultiUrlPickerData,
} from './modal-layout-multi-url-picker.element';

export default {
	title: 'API/Modals/Layouts/Multi Url Picker',
	component: 'umb-modal-layout-multi-url-picker',
	id: 'modal-layout-multi-url-picker',
} as Meta;

const data: UmbModalMultiUrlPickerData = {
	title: '',
	hideAnchor: false,
	selection: '',
};

export const Overview: Story<UmbModalLayoutMultiUrlPickerElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-modal-layout-multi-url-picker .data=${data as any}></umb-modal-layout-multi-url-picker>
`;
