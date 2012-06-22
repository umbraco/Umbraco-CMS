result = "<ul>";

currentPage.Children.each do |this_item|
  result += "<li><a href='" + this_item.NiceUrl + "'>" + this_item.Name + "</a></li>"
end

result += "</ul>"

puts result