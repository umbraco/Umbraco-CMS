import '../../../components/body-layout/body-layout.element.js';
import './icon-picker-modal.element.js';

import { Meta, Story } from '@storybook/web-components';
import type { UmbIconPickerModalElement } from './icon-picker-modal.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import { UmbIconPickerModalData } from '@umbraco-cms/backoffice/modal';

export default {
	title: 'API/Modals/Layouts/Icon Picker',
	component: 'umb-icon-picker-modal',
	id: 'umb-icon-picker-modal',
} as Meta;

const data: UmbIconPickerModalData = {
	color: undefined,
	icon: undefined,
};

export const Overview: Story<UmbIconPickerModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-icon-picker-modal .data=${data as any}></umb-icon-picker-modal>
`;
