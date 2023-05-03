import '../../components/body-layout/body-layout.element';
import './link-picker-modal.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbLinkPickerModalElement } from './link-picker-modal.element';

export default {
	title: 'API/Modals/Layouts/Link Picker',
	component: 'umb-link-picker-modal',
	id: 'umb-link-picker-modal',
} as Meta;

export const Overview: Story<UmbLinkPickerModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-link-picker-modal></umb-link-picker-modal>
`;
