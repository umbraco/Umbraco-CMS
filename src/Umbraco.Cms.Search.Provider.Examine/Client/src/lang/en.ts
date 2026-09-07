import type { UmbLocalizationDictionary } from '@umbraco-cms/backoffice/localization-api';

export default {
  searchExamine: {
    showFields: 'Show Fields',
    headline: 'Search Document Fields',
    filterPlaceholder: 'Filter fields by name or value...',
    filterLabel: 'Filter fields by name or value',
    fieldCount: (count: number) => {
      switch (count) {
        case 1:
          return '1 field';
        default:
          return `${count} fields`;
      }
    },
    tableColumnName: 'Name',
    tableColumnValue: 'Value',
    copyValue: 'Copy value',
    seeMore: 'See more',
    seeLess: 'See less',
    noFieldsMatch: 'No fields match your filter.',
    noFields: 'This document has no indexed fields.',
    loadError: 'Failed to load document fields. Please try again.',
    valueIndex: (index: number) => `Value ${index}`,
    fieldType: (type: string) => {
      switch (type) {
        case 'keywords':
          return 'Keyword (exact match)';
        case 'texts':
          return 'Full Text';
        case 'textsr1':
          return 'Full Text (Boost: High)';
        case 'textsr2':
          return 'Full Text (Boost: Medium)';
        case 'textsr3':
          return 'Full Text (Boost: Low)';
        case 'integers':
          return 'Integer';
        case 'decimals':
          return 'Decimal';
        case 'datetimeoffsets':
          return 'Date/Time';
        default:
          return type;
      }
    },
  },
} satisfies UmbLocalizationDictionary;
