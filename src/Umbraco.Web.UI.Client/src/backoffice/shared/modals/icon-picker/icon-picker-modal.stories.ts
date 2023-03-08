import '../../components/body-layout/body-layout.element';
import './icon-picker-modal.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbIconPickerModalElement } from './icon-picker-modal.element';
import { UmbIconPickerModalData } from '.';

export default {
	title: 'API/Modals/Layouts/Icon Picker',
	component: 'umb-icon-picker-modal',
	id: 'umb-icon-picker-modal',
} as Meta;

const data: UmbIconPickerModalData = {
	multiple: true,
	selection: [],
};

export const Overview: Story<UmbIconPickerModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-icon-picker-modal .data=${data as any}></umb-icon-picker-modal>
`;
