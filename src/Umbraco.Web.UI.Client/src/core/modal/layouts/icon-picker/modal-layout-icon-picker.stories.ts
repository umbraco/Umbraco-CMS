import '../../../../backoffice/shared/components/body-layout/body-layout.element';
import './modal-layout-icon-picker.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbModalLayoutIconPickerElement, UmbModalIconPickerData } from './modal-layout-icon-picker.element';

export default {
	title: 'API/Modals/Layouts/Icon Picker',
	component: 'umb-modal-layout-icon-picker',
	id: 'modal-layout-icon-picker',
} as Meta;

const data: UmbModalIconPickerData = {
	multiple: true,
	selection: [],
};

export const Overview: Story<UmbModalLayoutIconPickerElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-modal-layout-icon-picker .data=${data as any}></umb-modal-layout-icon-picker>
`;
