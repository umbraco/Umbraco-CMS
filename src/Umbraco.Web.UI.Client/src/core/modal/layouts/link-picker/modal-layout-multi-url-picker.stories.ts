import '../../../../backoffice/shared/components/body-layout/body-layout.element';
import './modal-layout-link-picker.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbModalLayoutLinkPickerElement } from './modal-layout-link-picker.element';

export default {
	title: 'API/Modals/Layouts/Link Picker',
	component: 'umb-modal-layout-link-picker',
	id: 'modal-layout-link-picker',
} as Meta;

export const Overview: Story<UmbModalLayoutLinkPickerElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-modal-layout-link-picker></umb-modal-layout-link-picker>
`;
