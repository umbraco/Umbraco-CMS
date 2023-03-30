import '../../../../shared/components/body-layout/body-layout.element';
import './document-type-picker-modal.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbDocumentTypePickerModalElement } from './document-type-picker-modal.element';
import type { UmbDocumentTypePickerModalData } from '@umbraco-cms/backoffice/modal';

export default {
	title: 'API/Modals/Layouts/Content Picker',
	component: 'umb-document-type-picker-modal',
	id: 'umb-document-type-picker-modal',
} as Meta;

const data: UmbDocumentTypePickerModalData = {
	multiple: true,
	selection: [],
};

export const Overview: Story<UmbDocumentTypePickerModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-document-picker-modal .data=${data as any}></umb-document-picker-modal>
`;
