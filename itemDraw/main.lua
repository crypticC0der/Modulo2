a = {{.5,.5},{.5,.2}}
height = math.sqrt(3)/2
width = 1

function tripoints()
	local w,h = love.graphics.getPixelDimensions()
	local scale=math.min(h,w);
	local scale=scale*.9
	local hp = (h - (height*scale))/2
	local wp = (w - width*scale)/2
	return wp + width*scale,hp + height*scale, --bottom right
		wp + width*scale/2,hp, -- top
		wp + 0,hp + height*scale --bottom left
end

function getLine(a, b)
	local i,j = a[1], a[2]
	local p,q = b[1], b[2]
	local d = 2*j - 2*q
	local m = 2*p-2*i
	m = m/d
	local c = (i^2 + j^2) - (p^2 + q^2)
	c = c/d
	return m, c
end

-- (i,j) (p,q)
--
function pointsToLines(a,b)
	local i,j=a[1],a[2]
	local p,q=b[1],b[2]
	local m = (q-j)/(p-i)
	-- j = i*m +c
	-- j - i*m
	return m, j-i*m
end

function isPointInPolygon(x, y, poly)
  local x1, y1, x2, y2
  local len = #poly
  x2, y2 = poly[len - 1], poly[len]
  local wn = 0
  for idx = 1, len, 2 do
    x1, y1 = x2, y2
    x2, y2 = poly[idx], poly[idx + 1]

    if y1 > y then
      if (y2 <= y) and (x1 - x) * (y2 - y) < (x2 - x) * (y1 - y) then
        wn = wn + 1
      end
    else
      if (y2 > y) and (x1 - x) * (y2 - y) > (x2 - x) * (y1 - y) then
        wn = wn - 1
      end
    end
  end
  return wn % 2 ~= 0 -- even/odd rule
end

function crossLines(m1,c1, m2,c2)
	local x = (c2 - c1)/(m1-m2)
	return x,m1*x + c1
end

-- the three lines are
-- x=0,
-- y=0
-- =1-x
function getPointsFromLine(m,c)
	points = {tripoints()}
	m1,c1 = pointsToLines({points[1],points[2]},{points[3],points[4]})
	m2,c2 = pointsToLines({points[1],points[2]},{points[5],points[6]})
	m3,c3 = pointsToLines({points[3],points[4]},{points[5],points[6]})
	p1 = {crossLines(m1,c1,m,c)}
	p2 = {crossLines(m2,c2,m,c)}
	p3 = {crossLines(m3,c3,m,c)}

	print(p1p3,p2p1,p2p3)
	return {p1,p3}
end


function baryToReal(p)
	local x,y = p[1],p[2]
	local p1x, p1y, p2x, p2y, p3x, p3y = tripoints()
	local rx = p1x*x +p2x*y + p3x*(1-(x+y))
	local ry = p1y*x +p2y*y + p3y*(1-(x+y))
	return rx,ry
end

function drawCircle(p,...)
	local px,py= baryToReal(p)
	love.graphics.setColor(...)
	love.graphics.circle('fill',px,py,5)
	love.graphics.setColor( 1, 1, 1, 1)
end

function love.draw()
	newa = {{baryToReal(a[1])},{baryToReal(a[2])}}
	newa[1][1], newa[1][2] = love.mouse.getPosition()
	love.graphics.polygon("line", tripoints())
	local m,c = getLine(newa[1],newa[2])
	local points = getPointsFromLine(m,c)
	drawCircle(a[2],0,1,0)
	love.graphics.line(points[1][1],points[1][2],points[2][1],points[2][2])
end
