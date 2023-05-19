import { Meta, StoryObj } from '@storybook/web-components';
import './variantable-property.element';
import type { UmbVariantablePropertyElement } from './variantable-property.element'

const meta: Meta<UmbVariantablePropertyElement> = {
    title: 'Components/Variantable Property',
    component: 'umb-variantable-property',
};
  
export default meta;
type Story = StoryObj<UmbVariantablePropertyElement>;

export const Overview: Story = {
    args: {
        property: {
            name: 'Header',
            alias: 'headerAlias',
            appearance: {
                labelOnTop: false
            },
            description: 'This is a description',
            variesByCulture: true,
            variesBySegment: true,
            validation: {
                mandatory: true,
                mandatoryMessage: 'This is a mandatory message',
            }
        },

    }
};

