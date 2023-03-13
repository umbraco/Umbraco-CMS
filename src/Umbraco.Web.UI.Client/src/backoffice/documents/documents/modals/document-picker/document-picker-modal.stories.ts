import '../../../../shared/components/body-layout/body-layout.element';
import './document-picker-modal.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbDocumentPickerModalElement } from './document-picker-modal.element';
import type { UmbDocumentPickerModalData } from './index';

export default {
	title: 'API/Modals/Layouts/Content Picker',
	component: 'umb-document-picker-modal',
	id: 'umb-document-picker-modal',
} as Meta;

const data: UmbDocumentPickerModalData = {
	multiple: true,
	selection: [],
};

export const Overview: Story<UmbDocumentPickerModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-document-picker-modal .data=${data as any}></umb-document-picker-modal>
`;
