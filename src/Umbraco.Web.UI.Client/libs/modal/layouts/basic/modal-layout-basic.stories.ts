import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import { UmbModalLayoutBasicElement, UmbBasicModalData } from './modal-layout-basic.element';

export default {
	title: 'API/Modals/Layouts/Basic',
	component: 'umb-modal-layout-basic',
	id: 'modal-layout-basic',
} as Meta;


const htmlContent = html`
	<uui-table aria-label="Example table" aria-describedby="#some-element-id">
		<!-- Apply styles to the uui-table-column to style the columns. You must have the same number of this elements as you have columns -->
		<uui-table-column style="width: 20%;"></uui-table-column>
		<uui-table-column style="width: 80%;"></uui-table-column>

		<uui-table-head>
			<uui-table-head-cell>Title 1</uui-table-head-cell>
			<uui-table-head-cell>Title 2</uui-table-head-cell>
		</uui-table-head>

		<uui-table-row>
			<uui-table-cell>Cell 1</uui-table-cell>
			<uui-table-cell>Cell 2</uui-table-cell>
		</uui-table-row>

		<uui-table-row>
			<uui-table-cell>Cell 3</uui-table-cell>
			<uui-table-cell>Cell 4</uui-table-cell>
		</uui-table-row>
	</uui-table>`;


const data: UmbBasicModalData = {
	header: html`<uui-icon name="umb:bug"></uui-icon> Debug: Contexts`,
	content: htmlContent,
	overlaySize: 'small'
};


export const Overview: Story<UmbModalLayoutBasicElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-modal-layout-basic .data=${data as any}></umb-modal-layout-basic>
`;