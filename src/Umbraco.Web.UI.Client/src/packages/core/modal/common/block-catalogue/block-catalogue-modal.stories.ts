import '../../../components/body-layout/body-layout.element.js';
import './block-catalogue-modal.element.js';

import { Meta, Story } from '@storybook/web-components';
import type { UmbBlockCatalogueModalElement } from './block-catalogue-modal.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'API/Modals/Layouts/Block Catalogue',
	component: 'umb-block-catalogue-modal',
	id: 'umb-block-catalogue-modal',
} as Meta;

export const Overview: Story<UmbBlockCatalogueModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-block-catalogue-modal></umb-block-catalogue-modal>
`;
