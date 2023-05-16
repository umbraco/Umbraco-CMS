import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbSectionMainElement } from './section-main.element';
import './section-main.element';

export default {
	title: 'Sections/Shared/Section Main',
	component: 'umb-section-main',
	id: 'umb-section-main',
} as Meta;

export const AAAOverview: Story<UmbSectionMainElement> = () =>
	html` <umb-section-main>Section Main Area</umb-section-main> `;
AAAOverview.storyName = 'Overview';
