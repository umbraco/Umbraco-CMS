import '../../components/body-layout/body-layout.element';
import './embedded-media-modal.element';

import { Meta } from '@storybook/web-components';
import { html } from 'lit';
import { UmbEmbeddedMediaModalData } from '@umbraco-cms/backoffice/modal';

export default {
	title: 'API/Modals/Layouts/Embedded Media',
	component: 'umb-embedded-media-modal',
	id: 'umb-embedded-media-modal',
} as Meta;

const data: UmbEmbeddedMediaModalData = {
	url: 'https://youtu.be/wJNbtYdr-Hg',
	width: 360,
	height: 240,
	constrain: true,
};

export const Overview = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-embedded-media-modal .data=${data as any}></umb-embedded-media-modal>
`;
