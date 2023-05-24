import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import type { UmbDataTypePickerFlowModalElement } from './data-type-picker-flow-modal.element.js';
import type { UmbPropertyEditorUIPickerModalData } from '@umbraco-cms/backoffice/modal';

import './data-type-picker-flow-modal.element';
import '../../../../core/components/body-layout/body-layout.element';

export default {
	title: 'API/Modals/Layouts/Data Type Picker Flow',
	component: 'umb-data-type-picker-flow-modal',
	id: 'umb-data-type-picker-flow-modal',
} as Meta;

const data: UmbPropertyEditorUIPickerModalData = { selection: [] };

export const Overview: Story<UmbDataTypePickerFlowModalElement> = () => html`
	<umb-data-type-picker-flow-modal .data=${data as any}></umb-data-type-picker-flow-modal>
`;
